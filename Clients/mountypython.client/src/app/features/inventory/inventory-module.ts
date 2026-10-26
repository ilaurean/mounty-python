import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { provideEffects } from '@ngrx/effects';
import { provideState } from '@ngrx/store';
import { SharedModule } from '../../shared/shared-module';
import { BoardForm } from './board-form';
import { BoardList } from './board-list';
import { BoardsEffects, boardsFeature } from './boards.store';

const routes: Routes = [{ path: '', component: BoardList }];

@NgModule({
  declarations: [BoardList, BoardForm],
  imports: [SharedModule, RouterModule.forChild(routes)],
  providers: [provideState(boardsFeature), provideEffects(BoardsEffects)]
})
export class InventoryModule { }
