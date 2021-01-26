/*for direct messages within groups*/

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from '../_helpers/paginate-helper';
import { Message } from '../_models/message';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { MembersService } from './members.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  allDirectMessagesSubject = new BehaviorSubject<Message[]>([]);
  allDirectMessages$ = this.allDirectMessagesSubject.asObservable();

  constructor(private http: HttpClient, private memberService:MembersService, private toastr: ToastrService) { }

  createHubConnection(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(this.hubUrl + 'directMessage?user=' + otherUsername, {
       accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection
      .start()
      .catch(error => console.log(error));
    
    this.hubConnection.on("receiveCurrentDMs", (currentMessages ) => {
      this.allDirectMessagesSubject.next(currentMessages.result);
    });

    this.hubConnection.on("newMessageReceived", (newMessage) => {
      this.allDirectMessages$.pipe(take(1)).subscribe(messages => {
        this.allDirectMessagesSubject.next([...messages, newMessage]);
      });
      /*ZASTO ovo ne radi a ovo gore radi pitat zvonimira */
      // this.allDirectMessages$.subscribe(messages => {
      //   this.allDirectMessagesSubject.next([...messages, newMessage]);
      // });
    });

    this.hubConnection.on("groupUpdated", ({name,connections}) => {
      console.log("MRKL");
      console.log(connections);
      console.log(name);
      if (connections.some(x => x.username === otherUsername)) {
        this.allDirectMessages$.pipe(take(1)).subscribe(messages => {
          messages.forEach(m => {
            if(!m.dateRead){
              m.dateRead = new Date(Date.now());
            }
          });
          this.allDirectMessagesSubject.next([...messages]);
        });
      }
    });
  }
  stopHubConnection(){
    if(this.hubConnection){
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }


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

  // it is live now, no post request (commented), use connection hub
  createMessage(username:string, content:string){
    //return this.http.post(environment.baseUrl+'messages',{
    return this.hubConnection.invoke("SendMessage", {
      recipientUserName: username,
      content: content
    }).catch(error => console.error(
      "Error u message.service funkcija createMessage: ", error));
    
    
  }
  deleteMessage(id : number){
    return this.http.delete(environment.baseUrl + 'messages/' + id.toString());
  }
}
