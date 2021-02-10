import { Message } from '@angular/compiler/src/i18n/i18n_ast';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { userInfo } from 'os';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { UserMessages } from '../_models/userMessages';
import { MessageService } from './message.service';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  currentUsersSubject = new BehaviorSubject<UserMessages[]>([]);
  currentUsers$ = this.currentUsersSubject.asObservable();

  currentUsers : string[] = [];
  constructor(private toastr: ToastrService, private router:Router) {}

  createHubConnection(user: User){
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build()

    this.hubConnection
      .start()
      .catch(error => console.log(error));
    
    

    this.hubConnection.on('userConnected', (username, numberUnreadMessages) => {
      this.toastr.info(username + ' has connected');
      console.log("kerum00000", username, numberUnreadMessages);

      this.currentUsers$.pipe(take(1)).subscribe( (UsernamesAndMessages : UserMessages[]) => {
        console.log("kerum", UsernamesAndMessages);
        const userMessagesToAdd : UserMessages = { username: username, unreadMsgs: numberUnreadMessages };
        
        this.currentUsersSubject.next([...UsernamesAndMessages, userMessagesToAdd])
      })
    });

    this.hubConnection.on('userDisconnected', username => {
      this.toastr.warning(username + ' has disconnected');
      this.currentUsers$.pipe(take(1)).subscribe((UserNameAndMessages : UserMessages[]) => {
        this.currentUsersSubject.next([...UserNameAndMessages.filter( UserMessageObject => UserMessageObject.username !== username)])
      })
    });

    this.hubConnection.on('GetOnlineUsers', (arrayOfOnlineUsers : string[], listOfUnreadNumberOrdered : number[]) => {
      console.log(arrayOfOnlineUsers, listOfUnreadNumberOrdered);
      let userMessages : UserMessages[] = [];
      // this.currentUsers$.pipe(take(1)).subscribe( (UsernamesAndMessages : UserMessages[]) => {
      //   userMessages = UsernamesAndMessages;
      // });
      
      for(let i:number = 0 ; i < arrayOfOnlineUsers.length; i+=1) {
        let userMessageToAdd : UserMessages = { username : arrayOfOnlineUsers[i], unreadMsgs : listOfUnreadNumberOrdered[i]};
        userMessages.push(userMessageToAdd);
      }
      this.currentUsersSubject.next(userMessages);
    });

    this.hubConnection.on("newMessageReceivedNotification", ({senderUsername, senderKnownAs, messagesUnreadReceived}) => {
      this.toastr.info("You have new message(s) from " + senderKnownAs + "(" +  messagesUnreadReceived + ")" )
      .onTap
      .pipe(take(1))
      .subscribe(() => this.router.navigateByUrl('/members/' + senderUsername + '?tab=3'));
    });
  }
  getConnectionState(){
    return this.hubConnection.state;
  }

  stopHubConnection(){
    this.hubConnection.stop().catch(error => console.log(error));
  }
}
