import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import {NgxGalleryOptions} from '@kolkov/ngx-gallery';
import {NgxGalleryImage} from '@kolkov/ngx-gallery';
import {NgxGalleryAnimation} from '@kolkov/ngx-gallery';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @Input() member! : Member;
  username! : string;

  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];

  constructor(private memberService : MembersService,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    // this.route.queryParams.subscribe(params => {
    // });
    let Username :string = this.route.snapshot.paramMap.get('username')!; //It tells TypeScript that even though something looks like it could be null, it can trust you that it's not:
    this.memberService.getMember(Username).subscribe(memberReturned =>{
      this.member = memberReturned;
      this.galleryImages = this.getImages();
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
  }
  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    
    for (const photo of this.member.photos) {
      console.log(photo);
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      })
    }
    return imageUrls;
  }

}
