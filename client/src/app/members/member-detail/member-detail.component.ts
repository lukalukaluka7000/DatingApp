import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import {NgxGalleryOptions} from '@kolkov/ngx-gallery';
import {NgxGalleryImage} from '@kolkov/ngx-gallery';
import {NgxGalleryAnimation} from '@kolkov/ngx-gallery';
import { ToastrService } from 'ngx-toastr';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';
import { PresenceService } from 'src/app/_services/presence.service';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @Input() member! : Member;
  @ViewChild('memberTabs', {static:true}) memberTabs : TabsetComponent;
  activeTab : TabDirective;
  //username! : string;
  user: User;
  messages:Partial<Message[]> = [];

  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];

  constructor(private memberService : MembersService,
    private route: ActivatedRoute,
    private acc: AccountService,
    private messageService:MessageService,
    private toastr: ToastrService,
    public presence : PresenceService,
    private router: Router) {
      this.acc.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
      this.router.routeReuseStrategy.shouldReuseRoute = () => false;
     }
  

  ngOnInit(): void {
    // this.route.queryParams.subscribe(params => {
    // });
    let Username :string = this.route.snapshot.paramMap.get('username')!; //It tells TypeScript that even though something looks like it could be null, it can trust you that it's not:
    this.route.data.subscribe(data => {
      this.member = data.member;
      //data.galleryImages;
    });
    // this.memberService.getMember(Username).subscribe(memberReturned =>{
    //   this.member = memberReturned;
    //   this.galleryImages = this.getImages();
    // });

    this.route.queryParams.subscribe(params => {
        params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    })


    this.galleryOptions=[
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview:false
      }
    ]
    this.galleryImages = this.getImages();
  }

  likeUser(member: Member) {
    this.memberService.addLike(member.username).subscribe(() => { //!!nas post u ms nista ne vrati pa je ()
      this.toastr.success("You have liked " + member.knownAs);
    })
  }

  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    
    for (const photo of this.member.photos) {
      if (photo.isApproved) {
        imageUrls.push({
          small: photo?.url,
          medium: photo?.url,
          big: photo?.url
        })
      }
    }
    return imageUrls;
  }
  loadMessageThread(){
    this.messageService.getMessageThread(this.member.username).subscribe(messages => {
      this.messages = messages;
    });
  }
  onTabActivated(data: TabDirective){
    this.activeTab = data;
    if(this.activeTab.heading === 'Messages' && this.messages.length===0){
      console.log("anica",this.user);
      this.messageService.createHubConnection(this.user, this.member.username);
      //this.loadMessageThread();
    }
    else{
      this.messageService.stopHubConnection();
    }
  }
  selectTab(tabId:number){
    this.memberTabs.tabs[tabId].active=true;
  }
  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
