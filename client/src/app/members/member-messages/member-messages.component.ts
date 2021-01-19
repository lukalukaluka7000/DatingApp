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
  @Input() messages : Message[];
  @Input() username: string;
  messageContent : string = 'Enter your message';
  constructor(private messageService : MessageService) { }

  ngOnInit(): void {
    
  }

  
  sendMessage(username, content){
    this.messageService.createMessage(username,content).subscribe((result:Message) => {
      console.log(222, result);
      this.messages.push(result);
      this.messageForm.reset();
    });
  }
}
