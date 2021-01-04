import { Component, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {}
  photourl: string = '';
  constructor(public accountService: AccountService,
    private router: Router,
    private toastr: ToastrService) {}
  

  ngOnInit() {
    // this.accountService.currentUser$.subscribe((user: User) => {
    //   this.photourl = user.photoUrl;
    // })
    // console.log("bekavac");
    // this.storageService.currentStorage$.subscribe((data) => {
    //   // this will call whenever your localStorage data changes
    //   // use localStorage code here and set your data here for ngFor
    //   console.log("cekavac");
    //   this.photourl = data.getItem('photoUrl')!;
    //   //this.photourl = data;
    //   console.log("aaa" + this.photourl);
    //   console.log(data);
    // });
  }


  login() {
    this.accountService.login(this.model).subscribe(response => {
      this.router.navigate(['/members']);
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigate(['/']);
  }
}