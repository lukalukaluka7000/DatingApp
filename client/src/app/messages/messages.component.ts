import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { ConfirmService } from '../_services/confirm.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages : Partial<Message[]>;
  pagination : Pagination;
  pageNumber:number;
  pageSize:number;
  container:string='Inbox';
  loading:boolean = false;
  constructor(private messageService : MessageService,
    private confirmService:ConfirmService) { }

  ngOnInit(): void {
    this.loadMessages();
  }
  loadMessages(){
    this.pageNumber=1;
    this.pageSize=4;
    this.loading = true;
    this.messageService.getMessages(this.pageNumber,this.pageSize,this.container).subscribe(response => {
      this.messages = response.result;
      this.pagination = response.pagination;
      this.loading=false;
    });
  }
  pageChanged(event:any){
    this.pageNumber=event.page;
    this.loadMessages();
  }
  onSendMessage(newContent){

  }
  onDeleteMessage(message: Message){
    this.confirmService.openModalWithComponent("Deleting message").subscribe(result => {
      if(result){
        this.messageService.deleteMessage(message.id).subscribe(() => {
          this.messages = this.messages.filter(m => m.id != message.id);
        })
      }
    });
    
  }
}
