import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { NgxGalleryThumbnailsComponent } from '@kolkov/ngx-gallery';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  
  uploader!: FileUploader;
  hasBaseDropzoneOver = false;
  baseUrl = environment.baseUrl;

  @Input() member!:Member;
  user!: User;

  constructor(private accountService : AccountService,
    private memberService: MembersService) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
    })
  }
  setMainPhotoEditor(photo : Photo){
    console.log("sssssss");
    this.memberService.setMainPhoto(photo.id).subscribe((text) =>{
      this.user.photoUrl = photo.url;
      this.accountService.setCurrentUser(this.user);

      this.member.photoUrl = photo.url;
      this.member.photos.forEach(value => {
        value.isMain = false;
      });
      let newMain = this.member.photos.find(f => f.id === photo.id);
      newMain!.isMain = true;

    });
    
  }
  deletePhotoEditor(photoId : number){
    this.memberService.deletePhoto(photoId).subscribe(() => {
      this.accountService.setCurrentUser(this.user);
      this.member.photos = this.member.photos.filter(f => f.id !== photoId);
    })
  }
  ngOnInit(): void {
    this.initializeUploader();
  }

  initializeUploader(){
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.user.token,  //provide token, not going through http interceptor
      //disableMultipart: true, // 'DisableMultipart' must be 'true' for formatDataFunction to be called.
      //formatDataFunctionIsAsync: true,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10*1024*1024
      }
    );

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item,response,status,headers) => {
      if(response){
        const photo : Photo= JSON.parse(response);
        this.member.photos.push(photo);
        if(photo.isMain){
          this.user.photoUrl = photo.url;
          this.member.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
        }
      }
    }
  }

  fileOverBase(event : any){
    this.hasBaseDropzoneOver = event;
  }

  

}
