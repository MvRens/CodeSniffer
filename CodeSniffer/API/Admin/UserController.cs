using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeSniffer.API.Definition;
using CodeSniffer.Auth;
using CodeSniffer.Repository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Admin
{
    [Route("/api/users")]
    [Authorize(Policy = CsPolicyNames.Admins)]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }


        [HttpGet]
        public async ValueTask<IEnumerable<ListUserViewModel>> List()
        {
            var definitions = await userRepository.List();
            var viewModels = definitions.Select(d => new ListUserViewModel(d.Id, d.Username, d.DisplayName));

            return viewModels;
        }


        [HttpGet("roles")]
        public IEnumerable<RoleViewModel> Roles()
        {
            return CsRoleNames.Info.Select(r => new RoleViewModel(r.Id, r.DisplayName));
        }



        [HttpGet("{id}")]
        public async ValueTask<ActionResult<DefinitionViewModel>> GetDetails(string id)
        {
            try
            {
                var details = await userRepository.GetDetails(id);

                return Ok(new UserViewModel(
                    details.Username,
                    details.DisplayName,
                    details.Email,
                    details.Role,
                    details.Notifications
                ));
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpPost]
        public async ValueTask<ActionResult<string>> InsertDetails([FromBody] UserInsertViewModel viewModel)
        {
            var user = ViewModelToUser(viewModel);
            var id = await userRepository.Insert(user, viewModel.Password, Request.Author());
            return Ok(id);
        }


        [HttpPut("{id}")]
        public async ValueTask<ActionResult> UpdateDetails(string id, [FromBody] UserUpdateViewModel viewModel)
        {
            var user = ViewModelToUser(viewModel);

            try
            {
                await userRepository.Update(id, user, viewModel.NewPassword, Request.Author());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        private static CsUser ViewModelToUser(BaseUserUpdateViewModel viewModel)
        {
            return new CsUser(
                viewModel.Username, 
                viewModel.DisplayName, 
                viewModel.Email, 
                viewModel.Role,
                viewModel.Notifications);
        }


        [HttpDelete("{id}")]
        public async ValueTask<ActionResult> Delete(string id)
        {
            // TODO prevent deletion of logged in user
            try
            {
                await userRepository.Remove(id, Request.Author());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
}
