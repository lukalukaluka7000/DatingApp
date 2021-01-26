/*for BROADCASTING messages LIVE CHAT*/

import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class MessageHubService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  //let map = new Map<string, string>();

  allLiveMessagesSubject = new BehaviorSubject<Map<string,string[]>>(new Map<string,string[]>());
  allLiveMessages$ = this.allLiveMessagesSubject.asObservable();

  constructor() { }

  getHubConnection(){
    return this.hubConnection;
  }

  createHubConnection(user: User){
    console.log("aperkat");
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(this.hubUrl + 'message', {
       accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection
      .start()
      .catch(error => console.log(error));
    
    this.hubConnection.on('Send', data => {  
        console.log(data);  
    });

    this.hubConnection.on('GetLiveMessagesInClient', (listOfLiveMessages) => {
      
      //let convertToMap = new Map<string,string[]>();
      console.log("isus");
      console.log(listOfLiveMessages.result);
      //listOfLiveMessages.result.forEach(live => convertToMap.set(live.key, live.value));
      
      //console.log("isus2", convertToMap);
      this.allLiveMessagesSubject.next(listOfLiveMessages.result);
      
    });
    console.log(this.hubConnection.state);
    this.hubConnection.on('SendDM', (message: string) => {
      console.log('SendDM AAAAA', message);  
    });

  }
  stopHubConnection(){
    console.log("aperkat2");
    if(this.hubConnection){
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }

  
}
