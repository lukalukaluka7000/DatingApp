import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { PaginatedResult } from "../_models/pagination";
import { UserParams } from "../_models/userParams";

export function getPaginatedResult<T>(url:string, params:HttpParams, http:HttpClient)  {
    const paginatedResult : PaginatedResult<T> = new PaginatedResult<T>();
    return http.get<T>(url , {observe: 'response', params}).pipe(
      map(response => {
        paginatedResult.result = response.body;
        if(response.headers.get("Pagination") !== null){
          paginatedResult.pagination = JSON.parse(response.headers.get("Pagination"));
        }
        return paginatedResult;
      })
    );
  }
export function getPaginationHeaders(userParams : UserParams){
    let params = new HttpParams();
    
    if(userParams.pageNumber !== null && userParams.pageSize !== null){
      params = params.append('pageNumber', userParams.pageNumber.toString());
      params = params.append('pageSize', userParams.pageSize.toString());
      console.log(params);
    }
    return params;
  }