import * as signalR from '@microsoft/signalr';

const HUB_URL = process.env.REACT_APP_HUB_URL || 'http://localhost:5001/hubs/garden';

let connection: signalR.HubConnection | null = null;

export const connectToHub = async (): Promise<signalR.HubConnection> => {
  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    return connection;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      skipNegotiation: false,
    })
    .withAutomaticReconnect()
    .build();

  await connection.start();
  return connection;
};

export const joinSession = async (sessionId: string): Promise<void> => {
  const conn = await connectToHub();
  await conn.invoke('JoinSession', sessionId);
};

export const onProcessingUpdate = (callback: (data: { status: string; message: string }) => void): void => {
  connection?.on('ProcessingUpdate', callback);
};

export const onProcessingComplete = (callback: (data: any) => void): void => {
  connection?.on('ProcessingComplete', callback);
};

export const onProcessingError = (callback: (data: { message: string }) => void): void => {
  connection?.on('ProcessingError', callback);
};

export const disconnect = async (): Promise<void> => {
  if (connection) {
    await connection.stop();
    connection = null;
  }
};
