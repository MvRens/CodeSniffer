using System.Diagnostics.CodeAnalysis;
using CodeSniffer.Repository.Users;
using JetBrains.Annotations;
using LiteDB;
using Serilog;

namespace CodeSniffer.Repository.LiteDB.Users
{
    public class LiteDbUserRepository : BaseLiteDbRepository, IUserRepository
    {
        private const string UserCollection = "User";
        private const string UserArchiveCollection = "UserArchive";


        public LiteDbUserRepository(ILiteDbConnectionPool connectionPool, ILiteDbConnectionString connectionString)
            : base(connectionPool, connectionString)
        {
        }


        
        public static ValueTask Initialize(ILiteDatabase database, ILogger logger)
        {
            var userCollection = database.GetCollection<UserRecord>(UserCollection);
            userCollection.EnsureIndex(r => r.Username);

            var archiveCollection = database.GetCollection<ArchivedUserRecord>(UserArchiveCollection);
            archiveCollection.EnsureIndex(r => r.OriginalId);


            // ReSharper disable once InvertIf
            if (userCollection.Count() == 0)
            {
                logger.Information("No users found, creating default admin user");
                userCollection.Insert(new UserRecord(ObjectId.NewObjectId(), "admin", "Admin", "admin@localhost",
                    HashPassword("admin"), "admin", false, "System", null));
            }

            return default;
        }


        public async ValueTask<IReadOnlyList<ListUser>> List()
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<UserListRecord>(UserCollection);

            return collection.FindAll()
                .Select(r => new ListUser(r.Id.ToString(), r.Username, r.DisplayName, r.Email))
                .ToList();
        }


        public async ValueTask<CsStoredUser> GetDetails(string id)
        {
            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<UserRecord>(UserCollection);

            var record = collection.FindById(recordId);
            if (record == null)
                throw new InvalidOperationException($"Unknown user Id: {id}");

            return MapUser(record);
        }


        public async ValueTask<LoginUser?> ValidateLogin(string username, string password)
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<UserRecord>(UserCollection);

            var record = collection.FindOne(r => r.Username == username);
            if (record == null)
                return null;

            if (!SamePassword(record.HashedPassword, password))
                return null;

            return new LoginUser(record.Username, record.DisplayName, record.Role);
        }


        public async ValueTask<string> Insert(CsUser newUser, string password, string author)
        {
            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<UserRecord>(UserCollection);

            var newRecord = MapUser(ObjectId.NewObjectId(), newUser, HashPassword(password), author, null);
            return collection.Insert(newRecord).ToString();
        }


        public async ValueTask Update(string id, CsUser newUser, string? password, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<UserRecord>(UserCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                throw new InvalidOperationException($"Unknown user Id: {id}");

            var newRecord = MapUser(recordId, newUser, HashPassword(password) ?? currentRecord.HashedPassword, author, null);
            if (!newRecord.Changed(currentRecord))
                return;


            var archiveCollection = connection.Database.GetCollection<ArchivedUserRecord>(UserArchiveCollection);
            archiveCollection.Insert(new ArchivedUserRecord(
                ObjectId.NewObjectId(),
                currentRecord.Id,
                currentRecord.Username,
                currentRecord.DisplayName,
                currentRecord.Email,
                currentRecord.Role,
                currentRecord.Notifications,
                currentRecord.Author,
                currentRecord.RemovedBy
            ));

            collection.Update(recordId, newRecord);
        }


        public async ValueTask Remove(string id, string author)
        {
            // TODO deal with concurrent edits

            var recordId = new ObjectId(id);

            using var connection = await GetConnection();
            var collection = connection.Database.GetCollection<UserRecord>(UserCollection);

            var currentRecord = collection.FindById(recordId);
            if (currentRecord == null)
                return;

            var removedRecord = new ArchivedUserRecord(
                ObjectId.NewObjectId(),

                currentRecord.Id,
                currentRecord.Username, 
                currentRecord.DisplayName,
                currentRecord.Email,
                currentRecord.Role,
                currentRecord.Notifications,
                currentRecord.Author,
                author);

            var archiveCollection = connection.Database.GetCollection<ArchivedUserRecord>(UserArchiveCollection);
            archiveCollection.Insert(removedRecord);


            collection.Delete(recordId);
        }


        private static CsStoredUser MapUser(UserRecord record)
        {
            return new CsStoredUser(
                record.Id.ToString(),
                record.Username,
                record.DisplayName,
                record.Email,
                record.Role,
                record.Notifications,
                record.Author
            );
        }


        private static UserRecord MapUser(ObjectId id, CsUser user, string password, string author, string? removedBy)
        {
            return new UserRecord(
                id,
                user.Username,
                user.DisplayName,
                user.Email,
                password,
                user.Role,
                user.Notifications,
                author,
                removedBy
            );
        }


        [return:NotNullIfNotNull("password")]
        private static string? HashPassword(string? password)
        {
            return password == null ? null : PBKDF2.Hash(password);
        }


        private static bool SamePassword(string hashedPassword, string password)
        {
            return PBKDF2.Validate(password, hashedPassword);
        }



        [UsedImplicitly]
        private class UserRecord
        {
            [BsonId] 
            public ObjectId Id { get; }

            public string Username { get; }
            public string DisplayName { get; }
            public string Email { get; }
            public string HashedPassword { get; }

            public string Role { get; }
            public bool Notifications { get; }

            public string Author { get; }
            public string? RemovedBy { get; }


            [BsonCtor]
            public UserRecord(ObjectId id, string username, string displayName, string email, string hashedPassword, string role, bool notifications, string author, string? removedBy)
            {
                Id = id;
                Username = username;
                DisplayName = displayName;
                Email = email;
                HashedPassword = hashedPassword;
                Role = role;
                Notifications = notifications;
                Author = author;
                RemovedBy = removedBy;
            }


            public bool Changed(UserRecord reference)
            {
                return !string.Equals(reference.Username, Username, StringComparison.InvariantCulture) ||
                       !string.Equals(reference.DisplayName, DisplayName, StringComparison.InvariantCulture) ||
                       !string.Equals(reference.Email, Email, StringComparison.InvariantCulture) ||
                       reference.Notifications != Notifications;
            }
        }


        [UsedImplicitly]
        private class ArchivedUserRecord : UserRecord
        {
            public ObjectId OriginalId { get; }

            [BsonCtor]
            public ArchivedUserRecord(ObjectId id, ObjectId originalId, string username, string displayName, string email, string role, bool notifications, string author, string? removedBy)
                : base(id, username, displayName, email, "<redacted>", role, notifications, author, removedBy)
            {
                OriginalId = originalId;
            }
        }


        [UsedImplicitly]
        private class UserListRecord
        {
            [BsonId]
            public ObjectId Id { get; }

            public string Username { get; }
            public string DisplayName { get; }
            public string Email { get; }


            [BsonCtor]
            public UserListRecord(ObjectId id, string username, string displayName, string email)
            {
                Id = id;
                Username = username;
                DisplayName = displayName;
                Email = email;
            }
        }
    }
}