import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { AuthService } from './core/auth.service';
import { SignalrService } from './core/signalr.service';
import { boardsActions } from './features/inventory/boards.store';
import { ordersActions } from './features/operations/orders.store';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly store = inject(Store);
  private readonly snackBar = inject(MatSnackBar);
  private readonly signalr = inject(SignalrService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly auth = inject(AuthService);

  ngOnInit(): void {
    this.signalr.board$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(board => {
      this.store.dispatch(boardsActions.boardUpserted({ board }));
      this.snackBar.open(`Board "${board.name}" was updated`, undefined, { duration: 4000 });
    });
    this.signalr.order$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(order => {
      this.store.dispatch(ordersActions.orderUpserted({ order }));
      this.snackBar.open(`Order "${order.name}" is now ${order.status}`, undefined, { duration: 4000 });
    });
    void this.signalr.start();
  }

  protected async logout(): Promise<void> {
    await this.signalr.stop();
    await this.auth.logout();
  }
}
