import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { env } from 'process';
import { BehaviorSubject, ReplaySubject, Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  
  private checkRolesUpdated = new BehaviorSubject<boolean>(false);
  currentInfo$ = this.checkRolesUpdated.asObservable();


  constructor(private http : HttpClient) { }

  getUsersWithRoles(){
    return this.http.get<Partial<User[]>>(environment.baseUrl + 'admin/users-with-roles');
  }

  setUserWithRoles(selectedRolesQuery : string, username: string){
    let filters = new HttpParams();
    filters = filters.append('roles', selectedRolesQuery);
    // Append here as much params as needed
    return this.http.post(environment.baseUrl + 'admin/edit-roles/' + username + '?roles=' + selectedRolesQuery, {});
  }

  sendNotif(notif:boolean){
    this.checkRolesUpdated.next(notif);
  }
  getUnApprovedPhotos(){
    return this.http.get<Partial<Photo[]>>(environment.baseUrl + 'admin/photos-to-moderate');
  }
  approvePhoto(photoId : number){
    return this.http.post(environment.baseUrl + 'admin/approve-photo/' + photoId, {}, {responseType:'text'});
  }
  rejectPhoto(photoId : number){
    return this.http.post(environment.baseUrl + 'admin/reject-photo/' + photoId, {}, {responseType: 'text'});
  }
}
