import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { DialogService } from 'src/app/_services/dialog.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild("editForm") editForm! : NgForm;
  user!: User;
  member!: Member;
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any){
    if(this.editForm.dirty){
      $event.returnValue = true;
    }
  }

  constructor(private accountService : AccountService,
    private memberService : MembersService,
    private toastrService:ToastrService,
    private dialogService: DialogService) { }

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
    });
    this.loadMember();
  }

  loadMember() {
    return this.memberService.getMember(this.user.username).subscribe(member => {
      this.member = member;
    })
  }

  updateMember(){
    console.log("member updating");
    console.log(this.member);
    this.memberService.updateMember(this.member).subscribe(() => {
      this.toastrService.success("Profile updated");
      this.editForm.reset(this.member);
    });

    
    
  }
  canDeactivate(): Observable<boolean> | boolean{
    if(this.editForm.dirty){
      return this.dialogService.confirm('Discard changes made?');
    }
    return true;
  }
}
