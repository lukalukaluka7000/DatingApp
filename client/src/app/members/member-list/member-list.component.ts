import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members$! : Observable<Member[]> | Observable<void>;
  m : Member[] = [];
  constructor(private memberService : MembersService) { }

  ngOnInit(): void {
    //this.retrieveMembers();
    this.members$ = this.memberService.getMembers();
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
