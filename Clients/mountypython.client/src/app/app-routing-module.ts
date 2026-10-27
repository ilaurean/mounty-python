import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'operations' },
  {
    path: 'inventory',
    loadChildren: () => import('./features/inventory/inventory-module').then(m => m.InventoryModule)
  },
  {
    path: 'operations',
    loadChildren: () => import('./features/operations/operations-module').then(m => m.OperationsModule)
  },
  { path: '**', redirectTo: 'operations' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
