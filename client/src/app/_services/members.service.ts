import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';


const httpOptions = {
  headers: new HttpHeaders()
}

@Injectable({
  providedIn: 'root'
})
export class MembersService  {
  members : Member[] = [];
  constructor(private http : HttpClient) {
    //httpOptions.headers = httpOptions.headers.set('Authorization', 'Bearer ' + JSON.parse(localStorage.getItem('user') || "")?.token);
   }
  
  getMembers() {
    console.log(this.members.length);
    if(this.members.length > 0) return of(this.members);
    return this.http.get<Member[]>(environment.baseUrl + 'users').pipe(
      map(members => {
        this.members = members;
        return members;
      })
    );
     //, httpOptions
  }

  getMember(username : string) {
    const member = this.members.find(x => x.username === username);
    if(member !== undefined){
      return of(member);
    }
    return this.http.get<Member>(environment.baseUrl + 'users/' + username);
  }

  updateMember(updatedMember: Member){
    let updatingItem = this.members.find(x => x.username === updatedMember.username && 
      x.id === updatedMember.id)!;
    let index = this.members.indexOf(updatingItem);
    this.members[index] = updatedMember;
    return this.http.put<Member>(environment.baseUrl + 'users', updatedMember);
  }

  setMainPhoto(photoId : number){
    return this.http.put(environment.baseUrl + 'users/set-main-photo/' + photoId, 
    {},
    {responseType:'text'}); // !!!! sada moze return Ok("nesto nesto")

      // {},
      // {params: {photoId: photoId.toString()}});
  }
  deletePhoto(photoId : number){
    return this.http.delete(environment.baseUrl + 'users/delete-photo/' + photoId);
  }
}
