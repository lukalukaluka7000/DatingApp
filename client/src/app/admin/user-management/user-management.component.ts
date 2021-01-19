import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { take } from 'rxjs/operators';
import { EditRoleModalContentComponent } from 'src/app/_modals/edit-role-modal-content/edit-role-modal-content.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  bsModalRef: BsModalRef;

  users: Partial<User[]>;

  constructor(private adminService : AdminService,
    private modalService : BsModalService,
    ) { 
      
    }

  ngOnInit(): void {
    // this.adminService.currentInfo$.subscribe(notif => {
    //   if(notif){
        
    //     this.getUsersAndRoles();
    //     this.adminService.sendNotif(false);
    //   }
    //   console.log("notif", notif);
    // });
    
    this.getUsersAndRoles();
  }

  getUsersAndRoles() {
    this.adminService.getUsersWithRoles().subscribe(users => {
      this.users = users;
      console.log(this.users);
    });
  }

  openRolesModal(user: User){
    //just open it
    const initialState = {
      class: 'modal-dialog-centered',
      list: [
        user
      ],
      title: 'Modal with component',
    };
    
    this.bsModalRef = this.modalService.show(EditRoleModalContentComponent, {initialState});
    this.bsModalRef.content.updatedSelectedRoles.subscribe(needsChange => {
      if(needsChange){
        this.getUsersAndRoles();
      }
    })
    this.bsModalRef.content.closeBtnName = 'Close';
    
  }
  getNotification(evt) {
    // Do something with the notification (evt) sent by the child!
    console.log("wswwswsw", evt);
    this.bsModalRef.content.closeBtnName = 'Close';
    this.getUsersAndRoles();
  }
}
