import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { Board, Order } from './models';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private readonly auth = inject(AuthService);
  private connection?: HubConnection;

  readonly board$ = new Subject<Board>();
  readonly order$ = new Subject<Order>();

  async start(): Promise<void> {
    if (this.connection) {
      return;
    }
    this.connection = new HubConnectionBuilder()
      .withUrl('/api/v1/updateshub', { accessTokenFactory: () => this.auth.getFreshToken() })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
    this.connection.on('Board', (board: Board) => this.board$.next(board));
    this.connection.on('Order', (order: Order) => this.order$.next(order));
    try {
      await this.connection.start();
    } catch (error) {
      console.error('SignalR connection failed', error);
    }
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
  }
}
