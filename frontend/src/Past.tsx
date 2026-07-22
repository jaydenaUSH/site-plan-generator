import Sidebar from './sidebar'
import { useEffect, useState } from 'react'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "./components/ui/card"
import { useNavigate } from 'react-router-dom'

function Past() {
    const navigator = useNavigate()
    const [drafts, setDrafts] = useState([])
    // Get All Drafts
    const apiBase = 'https://localhost:44306'

    //Get site plan req info

    //Change api url to get all instead of get one
    const getReqs = async () => {
        const response = await fetch(apiBase + '/api/requests/getAll', { headers: { "Accept": 'application/json' }, method: 'GET' });
        const data = await response.json();
        if (response.ok) {
            console.log(data)
         setDrafts(data)
        } else console.log("Error getting drafts")
    }
    useEffect(() => {
        getReqs()
    }, [])
    return (
        <div className="bg-background w-full h-screen flex flex-1 flex-row justify-between text-center relative ">
            <aside className="max-w-[15%] left-0 sticky top-0 h-screen">
                <Sidebar />
            </aside>
            <main className="flex-col flex-1  justify-center px-5">
                <h1 className='text-accent!'>View Past Drafts</h1>
                {drafts.length>0 && (
                    <div className= 'grid grid-cols-4 gap-5 '>
                        {drafts.map((draft) => (
                            <div key={draft.Id} >
                                <Card className='cursor-pointer' onClick={() => {
                                    navigator(`/requests/${draft.Id}`)
                                }}>
                                    <CardHeader><CardTitle>{draft.VenueName}</CardTitle>
                                    <CardDescription>{draft.Deadline.slice(0,10)}</CardDescription>
                                    </CardHeader>
                                    <CardContent>{draft.VenueAddress}</CardContent>

                                </Card>
                            </div>
                        ))}
                    </div>
                ) }
            </main>
        </div>
    )
}

export default Past;