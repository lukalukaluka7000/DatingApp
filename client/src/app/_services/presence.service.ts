import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  currentUsersSubject = new BehaviorSubject<string[]>([]);
  currentUsers$ = this.currentUsersSubject.asObservable();

  currentUsers : string[] = [];
  constructor(private toastr: ToastrService, private router:Router) { }

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

    this.hubConnection.on('userConnected', username => {
      this.toastr.info(username + ' has connected');
      this.currentUsers$.pipe(take(1)).subscribe(usernames => {
        this.currentUsersSubject.next([...usernames, username])
      })
    });

    this.hubConnection.on('userDisconnected', username => {
      this.toastr.warning(username + ' has disconnected');
      this.currentUsers$.pipe(take(1)).subscribe(usernames => {
        this.currentUsersSubject.next([...usernames.filter(x => x !== username)])
    })
    });

    this.hubConnection.on('GetOnlineUsers', (listOfOnlineUsers : string[]) => {
      this.currentUsersSubject.next(listOfOnlineUsers);
    });
    this.hubConnection.on("newMessageReceivedNotification", ({username,knownAs}) => {
      console.log("makljica", username);
      console.log("makljica", knownAs);

      this.toastr.info("You have new message from " + knownAs)
      .onTap
      .pipe(take(1))
      .subscribe(() => this.router.navigateByUrl('/members/' + username + '?tab=3'));
    });
  }
  getConnectionState(){
    return this.hubConnection.state;
  }

  stopHubConnection(){
    this.hubConnection.stop().catch(error => console.log(error));
  }
}
