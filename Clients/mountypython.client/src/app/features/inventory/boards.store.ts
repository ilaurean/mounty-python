import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { createActionGroup, createFeature, createReducer, emptyProps, on, props } from '@ngrx/store';
import { catchError, map, of, switchMap } from 'rxjs';
import { Board, BoardFilter, CreateBoardRequest, UpdateBoardRequest } from '../../core/models';
import { BoardsApi } from './board-list/boards.api';

export interface BoardsState {
  boards: Board[];
  loading: boolean;
  filter: BoardFilter;
}

export const initialBoardsState: BoardsState = {
  boards: [],
  loading: false,
  filter: { pageIndex: 0, pageSize: 20 }
};

export const boardsActions = createActionGroup({
  source: 'Boards',
  events: {
    load: props<{ filter: BoardFilter }>(),
    'load success': props<{ boards: Board[] }>(),
    create: props<{ request: CreateBoardRequest }>(),
    update: props<{ id: string; request: UpdateBoardRequest }>(),
    delete: props<{ id: string }>(),
    'delete success': props<{ id: string }>(),
    'board upserted': props<{ board: Board }>(),
    failure: emptyProps()
  }
});

export function upsertBoard(boards: Board[], board: Board): Board[] {
  const without = boards.filter(b => b.id !== board.id);
  if (board.deleteDate) {
    return without;
  }
  const exists = without.length !== boards.length;
  return exists ? boards.map(b => (b.id === board.id ? board : b)) : [...boards, board];
}

export const boardsFeature = createFeature({
  name: 'boards',
  reducer: createReducer(
    initialBoardsState,
    on(boardsActions.load, (state, { filter }) => ({ ...state, filter, loading: true })),
    on(boardsActions.loadSuccess, (state, { boards }) => ({ ...state, boards, loading: false })),
    on(boardsActions.deleteSuccess, (state, { id }) => ({ ...state, boards: state.boards.filter(b => b.id !== id) })),
    on(boardsActions.boardUpserted, (state, { board }) => ({ ...state, boards: upsertBoard(state.boards, board) })),
    on(boardsActions.failure, state => ({ ...state, loading: false }))
  )
});

@Injectable()
export class BoardsEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(BoardsApi);

  readonly load$ = createEffect(() =>
    this.actions$.pipe(
      ofType(boardsActions.load),
      switchMap(({ filter }) =>
        this.api.search(filter).pipe(
          map(boards => boardsActions.loadSuccess({ boards })),
          catchError(() => of(boardsActions.failure()))
        )
      )
    )
  );

  readonly create$ = createEffect(() =>
    this.actions$.pipe(
      ofType(boardsActions.create),
      switchMap(({ request }) =>
        this.api.create(request).pipe(
          map(board => boardsActions.boardUpserted({ board })),
          catchError(() => of(boardsActions.failure()))
        )
      )
    )
  );

  readonly update$ = createEffect(() =>
    this.actions$.pipe(
      ofType(boardsActions.update),
      switchMap(({ id, request }) =>
        this.api.update(id, request).pipe(
          map(board => boardsActions.boardUpserted({ board })),
          catchError(() => of(boardsActions.failure()))
        )
      )
    )
  );

  readonly delete$ = createEffect(() =>
    this.actions$.pipe(
      ofType(boardsActions.delete),
      switchMap(({ id }) =>
        this.api.delete(id).pipe(
          map(() => boardsActions.deleteSuccess({ id })),
          catchError(() => of(boardsActions.failure()))
        )
      )
    )
  );
}
