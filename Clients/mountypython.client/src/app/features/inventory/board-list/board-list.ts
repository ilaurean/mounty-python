import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { form } from '@angular/forms/signals';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
import { Store } from '@ngrx/store';
import { filter as filterOperator, tap } from 'rxjs';
import { AuthService } from '../../../core/auth.service';
import { Board, BoardDetailsData, BoardSearch } from '../../../core/models';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog';
import { BoardDetails } from '../board-details/board-details';
import { BoardForm, BoardFormData } from '../board-form/board-form';
import { boardsActions, boardsFeature } from '../boards.store';
import { BoardsApi } from '../boards.api';

@Component({
  selector: 'app-board-list',
  standalone: false,
  templateUrl: './board-list.html'
})
export class BoardList implements OnInit {
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly api = inject(BoardsApi);
  protected readonly auth = inject(AuthService);

  protected readonly boards = this.store.selectSignal(boardsFeature.selectBoards);
  protected readonly loading = this.store.selectSignal(boardsFeature.selectLoading);
  protected readonly currentFilter = this.store.selectSignal(boardsFeature.selectFilter);

  protected readonly search = signal<BoardSearch>({
    name: this.currentFilter().name ?? '',
    boardID: this.currentFilter().boardID ?? ''
  });
  protected readonly searchForm = form(this.search);

  protected readonly pageSize = computed(() => this.currentFilter().pageSize ?? 20);
  protected readonly pageIndex = computed(() => this.currentFilter().pageIndex ?? 0);
  protected readonly total = computed(() => {
    const count = this.boards().length;
    return this.pageIndex() * this.pageSize() + count + (count === this.pageSize() ? 1 : 0);
  });

  protected readonly columns = computed(() =>
    this.auth.isManager
      ? ['id', 'name', 'description', 'size', 'components', 'actions']
      : ['id', 'name', 'description', 'size', 'components']
  );

  ngOnInit(): void {
    this.load(this.pageIndex(), this.pageSize());
  }

  protected load(pageIndex: number, pageSize: number): void {
    const { name, boardID } = this.search();
    this.store.dispatch(boardsActions.load({ filter: { name, boardID, pageIndex, pageSize } }));
  }

  protected onSearch(): void {
    this.load(0, this.pageSize());
  }

  protected onPage(event: PageEvent): void {
    this.load(event.pageIndex, event.pageSize);
  }

  protected details(board: Board): void {
    this.dialog.open<BoardDetails, BoardDetailsData>(BoardDetails, { data: { board }, panelClass: 'app-dialog' });
  }

  protected create(): void {
    this.dialog
      .open<BoardForm, BoardFormData>(BoardForm, {
        data: {
          save: request => this.api.create(request, true).pipe(tap(board => this.store.dispatch(boardsActions.boardUpserted({ board }))))
        },
        panelClass: 'app-dialog'
      });
  }

  protected edit(board: Board): void {
    this.dialog
      .open<BoardForm, BoardFormData>(BoardForm, {
        data: {
          board,
          save: request => {
            const { id: _id, ...update } = request;
            return this.api.update(board.id, update, true).pipe(tap(updated => this.store.dispatch(boardsActions.boardUpserted({ board: updated }))));
          }
        },
        panelClass: 'app-dialog'
      });
  }

  protected remove(board: Board): void {
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, {
        data: { title: 'Delete board', message: `Delete board "${board.name}"?`, confirmLabel: 'Delete' }
      })
      .afterClosed()
      .pipe(filterOperator(confirmed => confirmed === true))
      .subscribe(() => this.store.dispatch(boardsActions.delete({ id: board.id })));
  }
}
