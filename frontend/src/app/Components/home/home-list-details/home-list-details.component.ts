import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-home-list-details',
  templateUrl: './home-list-details.component.html',
  styleUrls: ['./home-list-details.component.css']
})
export class HomeListDetailsComponent implements OnInit {
  @Input() pagenumber:number  
  constructor() { }

  ngOnInit() {
  }

}