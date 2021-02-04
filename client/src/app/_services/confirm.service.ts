import { Injectable, TemplateRef } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable, ObservableInput, of } from 'rxjs';
import { ConfirmDialogComponent } from '../_modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {

  modalRef: BsModalRef;
  message: string;
 
  constructor(private modalService: BsModalService) {}
 
  openModalWithComponent(title='Modal With Component', message='Are you sure?', ok='Ok', cancel='Cancel') : Observable<boolean> {
    const initialState = {
      title: title,
      message: message,
      btnOkText: ok,
      btnCancelText: cancel
    };
    this.modalRef = this.modalService.show(ConfirmDialogComponent, {initialState});

    //treba vratit iz ConfirmDialogComponent this.result
    return new Observable<boolean> (this.getResult());
  }
  private getResult(){
    return (observer) => {
      const subscription = this.modalRef.onHidden.subscribe(() => {
        observer.next(this.modalRef.content.result);
        observer.complete();
      })
      return {
        unsubscribe(){
          subscription.unsubscribe();
        }
      }
    }
  }
  // confirm(): Observable<boolean> {
  //   const confirmation = window.confirm(this.message || 'Are you sure?');

  //   this.message = 'Confirmed!';
  //   this.modalRef.hide();
  //   return of(confirmation);
  // }
 
  // decline(): void {
  //   this.message = 'Declined!';
  //   this.modalRef.hide();
  // }
}
