import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { MessageHubService } from './_services/message-hub.service';
import { PresenceService } from './_services/presence.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy{
  title = 'The Dating App';
  users: any;
  appLocalStorage : any;
  constructor(private http: HttpClient,
    private accountService : AccountService,
    private presence : PresenceService,
    private messageHub : MessageHubService
    ){} //private storageService : StorageService
  
  ngOnInit() {
    
    console.log("MORA UC UVID");
    //this.getUsers();
    // this.storageService.currentStorage$.subscribe(storage => {
    //   this.appLocalStorage = storage.getItem('user');
    //   console.log(this.appLocalStorage);
    // })
    this.setCurrentUser();
    //console.log("usa san");
  }
  ngOnDestroy() {
    this.messageHub.stopHubConnection();
  }

  setCurrentUser(){
    const user : User = JSON.parse(localStorage.getItem('user') || '{}');
    console.log("Jesus",user);
    if(user){
      this.accountService.setCurrentUser(user);
      
      this.presence.createHubConnection(user);
      console.log("maci");
      console.log(this.presence.getConnectionState());

      this.messageHub.createHubConnection(user);
    }
  }
}
