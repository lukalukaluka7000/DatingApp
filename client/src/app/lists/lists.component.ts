import { Component, OnInit } from '@angular/core';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Partial<Member[]>;
  predicate = 'liked';
  pagination = {} as Pagination;
  userParams = {} as UserParams;
  
  PAGE_SIZE : number = 3;
  constructor(private memberService : MembersService) { }

  ngOnInit(): void {
    //this.userParams = this.memberService.getUserParams();
    this.loadLikes();
  }

  loadLikes(){
    this.memberService.setPageParams(this.memberService.getUserParams().pageNumber, this.PAGE_SIZE); // pageNumber je varijabilan, PAGE_SIZE JE KONSTANTAN i o njemu odlucuje ovaj component
    this.memberService.getLikes(this.predicate, this.userParams.pageNumber, this.PAGE_SIZE).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination;
      this.userParams = this.memberService.getUserParams();
    });
  }
  pageChanged(event: any){
    this.userParams.pageNumber = event.page;
    this.loadLikes();
  }
}
