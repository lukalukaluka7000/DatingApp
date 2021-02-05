import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';
import { ConfirmService } from 'src/app/_services/confirm.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  unApprovedPhotos: Photo[] = [];
  constructor(private adminService: AdminService,
    private toastr: ToastrService,
    private confirmService : ConfirmService) { }

  ngOnInit(): void {
    this.adminService.getUnApprovedPhotos().subscribe(result => {
      if(result){
        this.unApprovedPhotos = result;
        
      }
    })
  }

  ApprovePhoto(photo: Photo){
    this.adminService.approvePhoto(photo.id).subscribe( (responseString) => {
      if(responseString.includes("successfuly approved")){
        // this.unApprovedPhotos.find(p => p.id == photo.id).isApproved = true;
        this.unApprovedPhotos = this.unApprovedPhotos.filter(f => f.id !== photo.id);
        this.toastr.info(responseString);
      }
      
    });
  }
  RejectPhoto(photo: Photo){
    this.confirmService.openModalWithComponent("Rejecting Photo", "Are you sure you that selected picture is inappropriate?","Yes","No").subscribe(result => {
      if (result) {
        this.adminService.rejectPhoto(photo.id).subscribe((responseString) => {
          if (responseString.includes("successfuly rejected")) {
            this.unApprovedPhotos = this.unApprovedPhotos.filter(f => f.id !== photo.id);
            this.toastr.warning(responseString);
          }
        })
      }
    });
    //dodaj confirm dialog
    // this.toastr.warning(responseString);
  }

}
