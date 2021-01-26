import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm : NgForm;
  //@Input() messages : Message[];
  messages : Message[];
  @Input() username: string;
  messageContent : string = 'Enter your message';
  constructor(public messageService : MessageService) { }

  ngOnInit(): void {
    this.messageService.allDirectMessages$.subscribe((currentMessages) => {
      this.messages = currentMessages;
    });
  }

  
  sendMessage(username, content){
    //invoke() ne vraca observable kao http.post pa se mijenja i ovdje
    //this.messageService.createMessage(username,content).subscribe((result:Message) => {
    this.messageService.createMessage(username, content).then(() =>{
      // this.messages.push(result); // vise ne pusham i mogu maknit property binding
      this.messageForm.reset();
    });
  }
}
