import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import {getPaginatedResult, getPaginationHeaders} from "../_helpers/paginate-helper";

const httpOptions = {
  headers: new HttpHeaders()
}

@Injectable({
  providedIn: 'root'
})
export class MembersService  {
  members : Member[] = [];
  user : User;
  userParams: UserParams;
  memberCache = new Map();

  constructor(private http : HttpClient, private accoutService:AccountService) {
    //httpOptions.headers = httpOptions.headers.set('Authorization', 'Bearer ' + JSON.parse(localStorage.getItem('user') || "")?.token);
    this.accoutService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(this.user);
      console.log(1122,this.userParams);
    })
  }

  setPageParams(pageNumber:number, pageSize:number){
    this.userParams.setPageParam(pageNumber,pageSize);
  }
  getUserParams(){
    return this.userParams;
  }
  resetUserParams(){
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }
  getMembers(userParams: UserParams) {
    var response = this.memberCache.get(Object.values(userParams).join('-'));
    if(response){
      return of(response);
    }

    console.log(Object.values(userParams).join('-'));

    let params = getPaginationHeaders(userParams);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    // if(this.members.length > 0) return of(this.members);
    return getPaginatedResult<Member[]>(environment.baseUrl + 'users', params, this.http).pipe(
      map(responseResult => {
        this.memberCache.set(Object.values(userParams).join('-'), responseResult);
        return responseResult;
      })
    )

  }

  getMember(username : string) {
    
    //get all the values from memberCache
    const member = [...this.memberCache.values()]
      .reduce((prev, curr) => prev.concat(curr.result), [])
      .find(member => member.username === username);
    //console.log(member); //imam result od paginacije sa 5 membera, another query nadoda jos jedan PaginatedResult i svaki od njih ima usera
    
    //ja ocu samo jedan array za nac nekog usera
    //rjsenje : reduce , reduce poziva callbackFn na svaki element, meni je svaki element PaginatedResult,
    //u callback fn je akumulirani result koji je dostupan u sljedecem iteru te funkcije
    if(member){
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

  
  addLike(username : string){
    return this.http.post(environment.baseUrl + 'likes/' + username, {});
  }
  getLikes(predicate: string, pageNumber:number, pageSize:number){
    const paginatedResult : PaginatedResult<Partial<Member[]>> = new PaginatedResult<Partial<Member[]>>();
    

    //this.setPageParams(pageNumber, pageSize);

    let params = getPaginationHeaders(this.userParams);

    params = params.append('predicate', predicate);

    return this.http.get<Partial<Member[]>>(
      environment.baseUrl + 'likes', //?predicate=' + 
      //predicate //+ '?pageNumber=' + pageNumber,
      {observe: 'response', params}).
    pipe(
      map(response => {
        paginatedResult.result = response.body;
        console.log(response.headers.get("Pagination"));
        
        if(response.headers.get("Pagination") !== null){
          console.log(222)
          paginatedResult.pagination = JSON.parse(response.headers.get("Pagination"));
          console.log(paginatedResult.pagination);
        }
        return paginatedResult;
      }
    ));
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
