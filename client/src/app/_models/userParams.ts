import { User } from "./user";

export class UserParams{
    gender: string;
    minAge: number = 18;
    maxAge: number = 99;
    pageNumber: number = 1;
    pageSize: number = 5;
    orderBy: string = 'lastActive';

    constructor(currentUser: User){
        console.log("macijajajajajajja", currentUser.gender);
        if(currentUser.gender === 'male'){
            this.gender = 'female';
        }
        else{
            this.gender = 'male';
        }

    }

}
