import { ThrowStmt } from '@angular/compiler';
import { Component, OnInit } from '@angular/core';
import { create } from 'domain';
import { PaginationComponent } from 'ngx-bootstrap/pagination';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageHubService } from 'src/app/_services/message-hub.service';
import { PresenceService } from 'src/app/_services/presence.service';
@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members$! : Observable<Member[]> | Observable<void>;
  members: Member[];
  m : Member[] = [];
  user:User;

  userParams = {} as UserParams;
  pagination = {} as Pagination;

  genderList = [{ key: 'Males', value: 'male' }, { key: 'Females', value: 'female' }];
  genderAfterLogin: string;

  PAGE_SIZE:number = 5;

  messageContent:string;
  currentLiveMessages : Map<string,string[]> = new Map<string,string[]>();

  currentUsers : string[] = [];
  constructor(private memberService : MembersService,
    public accountService : AccountService,
    public messageHubService : MessageHubService,
    private presence : PresenceService) {}

  ngOnInit(): void {
    //this.retrieveMembers();
    //this.members$ = this.memberService.getMembers();
    this.memberService.setPageParams(1, this.PAGE_SIZE);
    this.userParams = this.memberService.getUserParams();
    this.loadMembers();

    // this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
    //   this.user = user;
    // });
    // this.presence.currentUsers$.subscribe(receivedUsers => {
    //   this.currentUsers = receivedUsers;
    //   this.currentUsers.push(this.user.username);
    // });
    // this.presence.currentUsersSubject.next(this.currentUsers);

    this.messageHubService.allLiveMessagesSubject.subscribe( incomingData => {
      this.currentLiveMessages = incomingData;
      console.log(this.currentLiveMessages);
    });
    
  }
  loadMembers(){
    this.memberService.getMembers(this.userParams).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination;
    })
  }
  pageChanged(event: any){
    this.userParams.pageNumber = event.page;
    this.loadMembers();
  }
  resetFilters(){ // ali za this component
    this.userParams = this.memberService.resetUserParams();
    this.memberService.setPageParams(1, this.PAGE_SIZE);
    this.loadMembers();
  }

  sendMessage(content){
    this.sendDirectMessage(content);
    console.log("anja");
    // this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
    //   this.user = user;
    // });

    // this.messageHubService.allLiveMessages$.subscribe( (receivedMap : Map<string,string>) => {
    //   this.currentLiveMessages = receivedMap;
    // })
    // this.currentLiveMessages.set(content, this.user.username);

    // this.messageHubService.allLiveMessagesSubject.next(this.currentLiveMessages);
  }

  sendDirectMessage(message: string) {
    

    //console.log(this.hubConnection.state);
    this.messageHubService.getHubConnection().invoke('SendDirectMessage', message).then(
      () => console.log("Resolved!")
    ).catch(function(error) { 
      // error handler is called
      console.log(error); 
   });
      // .catch(err => console.error(err));
    //console.log(this.hubConnection.state);
    //this.stopHubConnection();
      // if(this.hubConnection.invoke('SendDirectMessage', message)){
      //   //if passed push or next to subject in messageHubSubject
      //   //this.allLiveMessagesSubject.next() //ipak ne ovdi nego tamo di se sastavljaju poruke
      //   let returned = this.hubConnection.invoke('GetLiveMessages');
      //   console.log("CCCCCAAAA",returned);

      // }
}
  // retrieveMembers(){
    
  //   return this.memberService.getMembers().subscribe(response => {
  //     console.log("aaa" + response);
  //     if(response) {
  //       this.members = response;
  //     }
  //   }, error => {
  //     console.log("Error in retrieveing members Member List Component");
  //     console.log(error);
  //   });
  // }
}
