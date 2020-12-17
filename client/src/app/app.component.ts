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
   
  constructor(private http: HttpClient,
    private accountService : AccountService){}
  
  ngOnInit() {
    this.getUsers();
    this.setCurrentUser();
    console.log("usa san");
  }

  setCurrentUser(){
    const user : User = JSON.parse(localStorage.getItem('user') || '{}');
    this.accountService.setCurrentUser(user);
  }

  getUsers(){
    console.log("ngOnInit AppComponent");
    
    //this.http.get('https://localhost:44347/api/users').subscribe(response => {
    //this.http.get('http://localhost:5001/api/users').subscribe(response => {
    this.http.get('http://localhost:18004/api/users').subscribe(response => {
      this.users = response;
    }, error => {
      console.log("usa u error od njega");
      console.log("tekst od errora: ", error);
    })
  }

}
