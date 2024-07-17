'use server'

import { Auction, PageResult } from "@/types";

export async function getData(query: string): Promise<PageResult<Auction>> {
  console.log(query)
    const res = await fetch(`http://localhost:6001/search?${query}`)
  
    if (!res.ok) throw new Error("Failed to fetch data")
    
    return res.json();
  }