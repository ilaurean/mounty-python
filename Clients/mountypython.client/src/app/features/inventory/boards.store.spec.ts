import { Board } from '../../core/models';
import { boardsActions, boardsFeature, initialBoardsState, upsertBoard } from './boards.store';

const board = (id: string, name = id, deleteDate?: string): Board => ({
  pk: `pk-${id}`,
  id,
  name,
  components: [],
  deleteDate
});

describe('boards store', () => {
  const reducer = boardsFeature.reducer;

  it('stores loaded boards and clears loading', () => {
    const loading = reducer(initialBoardsState, boardsActions.load({ filter: { name: 'a' } }));
    expect(loading.loading).toBe(true);

    const loaded = reducer(loading, boardsActions.loadSuccess({ boards: [board('B1')] }));
    expect(loaded.loading).toBe(false);
    expect(loaded.boards.map(b => b.id)).toEqual(['B1']);
  });

  it('adds a new board and replaces an existing one on upsert', () => {
    const added = upsertBoard([board('B1')], board('B2'));
    expect(added.map(b => b.id)).toEqual(['B1', 'B2']);

    const replaced = upsertBoard(added, board('B1', 'renamed'));
    expect(replaced.find(b => b.id === 'B1')?.name).toBe('renamed');
    expect(replaced.length).toBe(2);
  });

  it('removes a board that was deleted', () => {
    expect(upsertBoard([board('B1')], board('B1', 'B1', '2026-01-01')).length).toBe(0);
  });

  it('removes a board on delete success', () => {
    const state = { ...initialBoardsState, boards: [board('B1'), board('B2')] };
    const next = reducer(state, boardsActions.deleteSuccess({ id: 'B1' }));
    expect(next.boards.map(b => b.id)).toEqual(['B2']);
  });
});
