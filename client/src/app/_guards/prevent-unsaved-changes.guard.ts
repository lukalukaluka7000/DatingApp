import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';
import { ConfirmService } from '../_services/confirm.service';

export interface CanComponentDeactivate{
  canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}
@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<CanComponentDeactivate> {

  constructor(private confirmService: ConfirmService){}
  
  canDeactivate(component: CanComponentDeactivate, 
    route: ActivatedRouteSnapshot, 
    state: RouterStateSnapshot) {

    let url: string = state.url;
    console.log('Url: '+ url);

    return component.canDeactivate ? component.canDeactivate() : true;
  }
  
}
