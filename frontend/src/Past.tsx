import Sidebar from './sidebar'
import { useEffect } from 'react'
function Past() {
    // Get All Drafts
    const apiBase = 'https://localhost:44306'

    //Change api url to get all instead of get one
    const getDrafts = async () => {
        const response = await fetch(apiBase + '/api/review/2/draft', { headers: { "Accept": 'application/json' }, method: 'GET' });
        const data = await response.json();
        if (response.ok) {
            console.log(data)
        } else console.log("Error getting drafts")
    }
    useEffect(() => {
        getDrafts()
    }, [])
    return (
        <div className="bg-background w-full h-screen flex flex-1 flex-row justify-between text-center relative ">
            <aside className="max-w-[15%] left-0 sticky top-0 h-screen">
                <Sidebar />
            </aside>
            <main className="flex-col flex-1  justify-center px-5">
                <h1 className='text-accent!'>View Past Drafts</h1>
            </main>
        </div>
    )
}

export default Past;