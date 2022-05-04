import AWN from 'awesome-notifications';


const notifier = new AWN({
  durations: {
    global: 2500
  },
  labels: {
    success: '',
    info: '',
    warning: '',
    alert: ''
  },
  icons: {
    enabled: false
  }
} as any);


export function useNotifications(): Notifications
{
  return {
    success,
    info,
    warning,
    alert
  };
}


export interface Notifications
{
  success(message: string): void;
  info(message: string): void;
  warning(message: string): void;
  alert(message: string): void;
}



function success(message: string)
{
  notifier.success(message);
}

function info(message: string)
{
  notifier.info(message);
}

function warning(message: string)
{
  notifier.warning(message);
}

function alert(message: string)
{
  notifier.alert(message);
}