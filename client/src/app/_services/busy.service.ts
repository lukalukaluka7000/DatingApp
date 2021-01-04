import { Injectable } from '@angular/core';
//import { NgxSpinnerService } from 'ngx-spinner/public_api';
import { NgxSpinnerService } from "ngx-spinner";

@Injectable({
  providedIn: 'root'
})
export class BusyService  {
  constructor(private spinner: NgxSpinnerService) {}
 
  busy(){
    /** spinner starts on init */
    this.spinner.show();
  }
  idle(){
    this.spinner.hide();
  }
}