import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { AdminPanelComponent } from '../admin/admin-panel/admin-panel.component';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHaveRole]'
})
export class HaveRoleDirective implements OnInit {
  @Input() appHaveRole : string[];
  user : User;
  //@ViewChild('adminTemplate', { read: TemplateRef }) adminTemplate:TemplateRef<AdminPanelComponent>;
  
  constructor(private viewContainerRef : ViewContainerRef,
    private adminTemplateRef : TemplateRef<any>,
    private accountService : AccountService) {
    // elemRef.nativeElement.style.background = 'red';
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
    })
   }
  ngOnInit(): void {
    console.log("AAAAAA",this.adminTemplateRef);
    if (this.user == null || this.appHaveRole.length == 0){
      this.viewContainerRef.clear();
      return;
    }
    
    if (this.user?.roles.some(r => this.appHaveRole.includes(r))){
      this.viewContainerRef.createEmbeddedView(this.adminTemplateRef);
    }
    else{
      this.viewContainerRef.clear();
    }
    
  }


}
