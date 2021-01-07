import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from 'ngx-bootstrap/pagination';
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { UserParams } from 'src/app/_models/userParams';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members$! : Observable<Member[]> | Observable<void>;
  members: Member[];
  m : Member[] = [];

  userParams = {} as UserParams;
  pagination = {} as Pagination;

  genderList = [{ key: 'Males', value: 'male' }, { key: 'Females', value: 'female' }];
  genderAfterLogin: string;

  constructor(private memberService : MembersService) {
    
   }

  ngOnInit(): void {
    //this.retrieveMembers();
    //this.members$ = this.memberService.getMembers();
    
    this.userParams = this.memberService.getUserParams();
    this.loadMembers();
    
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
  resetFilters(){
    this.userParams = this.memberService.resetUserParams();
    this.loadMembers();
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
