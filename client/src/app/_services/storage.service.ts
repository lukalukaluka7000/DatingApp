// import { Injectable } from '@angular/core';
// import { Observable, ReplaySubject } from 'rxjs';

// @Injectable({
//   providedIn: 'root'
// })
// export class StorageService {

//   private storageSubscription = new ReplaySubject<Storage>(1);
//   currentStorage$ = this.storageSubscription.asObservable();

//   // watchStorage() : Observable<any>{
//   //   return this.storageSubscription.asObservable();
//   // }

//   setItem(key: string, data: any) {
//     localStorage.setItem(key, data);
//     this.storageSubscription.next(localStorage);
//   }

//   removeItem(key:string) {
//     localStorage.removeItem(key);
//     this.storageSubscription.next();
//   }
// }
