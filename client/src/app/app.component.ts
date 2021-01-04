import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'The Dating App';
  users: any;
  appLocalStorage : any;
  constructor(private http: HttpClient,
    private accountService : AccountService,
    ){} //private storageService : StorageService
  
  ngOnInit() {
    //this.getUsers();
    // this.storageService.currentStorage$.subscribe(storage => {
    //   this.appLocalStorage = storage.getItem('user');
    //   console.log(this.appLocalStorage);
    // })
    this.setCurrentUser();
    //console.log("usa san");
  }

  setCurrentUser(){
    const user : User = JSON.parse(localStorage.getItem('user') || '{}');
    this.accountService.setCurrentUser(user);
  }
}
