import { Component, EventEmitter, OnInit, Output } from '@angular/core';

import { BsModalRef } from 'ngx-bootstrap/modal';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-edit-role-modal-content',
  templateUrl: './edit-role-modal-content.component.html',
  styleUrls: ['./edit-role-modal-content.component.css']
})
export class EditRoleModalContentComponent implements OnInit {
  @Output() updatedSelectedRoles : EventEmitter<any> = new EventEmitter();
  title: string;
  closeBtnName: string;
  list: any[] = [];

  user : any; // ovo je modal property forwardan iz usermanagmenta
  rolesCurrentUser: any[] = [];
  options = [
    {name:'Admin', checked:false},
    {name:'Moderator', checked:false},
    {name:'Member', checked:false}
  ]
  rolesAll: any[] = ["Admin", "Moderator", "Member"]

  constructor(public bsModalRef: BsModalRef,
    private adminService : AdminService) {}
 
  ngOnInit() {
    this.options = this.resetOptions();
    this.rolesCurrentUser = [];
    
    this.user = this.list[0];

    this.rolesCurrentUser = this.user.roles;
    this.options.forEach(r => r.checked = true ? this.rolesCurrentUser.includes(r.name) : false);

  }
  submitRoles(){
    let selectedRoles = [];
    this.options.forEach( role => role.checked === true ? selectedRoles.push(role.name) : null );
    
    let selectedRolesQuery : string = selectedRoles.join(',');

    this.adminService.setUserWithRoles(selectedRolesQuery, this.user.username).subscribe( (success) => {
      // this.updatedSelectedRoles.emit( true );
      this.bsModalRef.hide();

      this.updatedSelectedRoles.emit(true);
      //this.adminService.sendNotif(true);
    });
  }
  resetOptions() {
    return [
      {name:'Admin', checked:false},
      {name:'Moderator', checked:false},
      {name:'Member', checked:false}
    ];
  }
}
