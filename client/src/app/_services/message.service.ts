import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from '../_helpers/paginate-helper';
import { Message } from '../_models/message';
import { PaginatedResult } from '../_models/pagination';
import { MembersService } from './members.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(private http: HttpClient, private memberService:MembersService) { }

  getMessages(pageNumber, pageSize, container){
    //const paginatedResult : PaginatedResult<Partial<Message[]>> = new PaginatedResult<Partial<Message[]>>();

    this.memberService.setPageParams(pageNumber, pageSize);

    let params = getPaginationHeaders(this.memberService.getUserParams());

    params = params.append('container', container);

    return getPaginatedResult<Partial<Message[]>>(environment.baseUrl + 'messages',params,this.http);
    /*return this.http.get<Partial<Message[]>>(
      environment.baseUrl + 'messages',
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
    ));*/
  }
  getMessageThread(username:string){
    return this.http.get<Partial<Message[]>>(environment.baseUrl + 'messages/thread/' + username);
  }
  createMessage(username:string, content:string){
    return this.http.post(environment.baseUrl+'messages',{
      recipientUserName: username,
      content: content
    });
  }
  deleteMessage(id : number){
    return this.http.delete(environment.baseUrl + 'messages/' + id.toString());
  }
}
