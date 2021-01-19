import { User } from "./user";

export class UserParams{
    gender: string;
    minAge: number = 18;
    maxAge: number = 99;
    pageNumber: number = 1;
    pageSize: number = 3;
    orderBy: string = 'lastActive';

    constructor(currentUser: User){
        if(currentUser.gender === 'male'){
            this.gender = 'female';
        }
        else{
            this.gender = 'male';
        }

    }
    setPageParam(pn:number, ps:number){
        this.pageSize=ps;
        this.pageNumber=pn;
    }
}
