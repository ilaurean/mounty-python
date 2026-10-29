import './app-module';
import { TestBed } from '@angular/core/testing';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterModule, provideRouter } from '@angular/router';
import { provideStore } from '@ngrx/store';
import { Subject } from 'rxjs';
import { App } from './app';
import { AuthService } from './core/auth.service';
import { Board, Order } from './core/models';
import { SignalrService } from './core/signalr.service';

describe('App', () => {
  const board$ = new Subject<Board>();
  const order$ = new Subject<Order>();
  const open = vi.fn();
  const start = vi.fn().mockResolvedValue(undefined);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [App],
      imports: [RouterModule, MatButtonModule, MatIconModule, MatToolbarModule],
      providers: [
        provideRouter([]),
        provideStore(),
        { provide: AuthService, useValue: { username: 'TestManager', isManager: true, isCustomer: false } },
        { provide: SignalrService, useValue: { board$, order$, start, stop: vi.fn() } },
        { provide: MatSnackBar, useValue: { open } }
      ]
    }).compileComponents();
  });

  it('creates the shell, starts SignalR and toasts incoming order updates', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    expect(start).toHaveBeenCalled();
    order$.next({ name: 'Order 1', status: 'Processing' } as Order);
    expect(open).toHaveBeenCalledWith('Order "Order 1" is now Processing', undefined, { duration: 4000 });
  });
});
