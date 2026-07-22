import Sidebar from './sidebar'
import { useParams } from 'react-router-dom'
import { useState, useEffect } from 'react'
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from './components/ui/card'
import { Separator } from './components/ui/separator'
import { MapPin, TriangleAlert } from 'lucide-react'
import { Badge } from "./components/ui/badge"



function Request() {
    const { id } = useParams()
    const apiBase = 'https://localhost:44306'
    const [drafts, setDrafts] = useState([])
const [req,setReq] = useState()


    //Get Drafts related to this ID
    const getDrafts = async () => {
        const response = await fetch(apiBase + `/api/review/getAllRequestDrafts/${id}`, { headers: { "Accept": 'application/json' }, method: 'GET' });
        const data = await response.json()
        if (response.ok) {
            console.log(data)
            setDrafts(data)
        } else {
            console.log(data);
        }
    }

    //get this venue site request info
    const getReq = async () => {
        const response = await fetch(apiBase + `/api/requests/${id}`, { headers: { "Accept": 'application/json' }, method: 'GET' });
        const data = await response.json()
        if (response.ok) {
            setReq(data)
            console.log(data)
        }
        else {
            console.log(data)     
              }
    }

    useEffect(() => { 
            getReq()
            getDrafts();
        }, [])
    return (
        <div className="bg-background w-full h-screen flex flex-1 flex-row justify-between text-center relative ">
            <aside className="max-w-[15%] left-0 sticky top-0 h-screen">
                <Sidebar />
            </aside>
            {req &&(<main className="flex-col flex-1  justify-center px-5">
               <h1>{req.VenueName}</h1>
                <div>
                    <h2>Initial Request</h2>
                    <Card>
                        <CardHeader className='flex justify-between pb-5'>
                        <div className= 'flex'>
                         <MapPin /> 
                                <p>{req.VenueAddress}</p></div>
                            <Badge variant='destructive'>{new Date((req.Deadline).slice(0,10)).toLocaleDateString()}</Badge>
                        </CardHeader>
                        <CardContent>
                            <div className='flex justify-between px-15'>
                                <div>
                                    <h2> Meal Package Goal</h2>
                                    <p>{req.MealPackageGoal}</p>
</div>
                                <div>
                                    <h2> Number of Lines</h2>

                                    <p>{req.NumberofLines}</p>
</div>
                                <div>
                                    <h2> Volunteer Count</h2>

                                    <p>{req.VolunteerCount}</p>
</div>

                            </div>
                            <Separator className='my-5'/>
                            <div className='grid grid-cols-2 gap-y-5'>
                                <div >
                                    <h2> Available Equipment</h2>
                                    <p>{req.AvaialableEquipment}</p></div>
                                <div>
                                    <h2> Loading Notes</h2>
                                    <p>{req.LoadingNotes}</p></div>
                                <div>
                                    <h2> Room Space Notes</h2>
                                    <p>{req.RoomSpaceNotes}</p></div>
                                <div>
                                    <h2> Additional Notes</h2>
                                    <p>{req.AdditionalNotes}</p></div>
                                
                               
                                

                               
                                


                            </div>   
                            <Separator className='mt-5' />

                        </CardContent>
                        <CardFooter className="flex justify-center">

                            <TriangleAlert stroke={"#EED202"} />
                            <p>{req.SpecialConstraints}</p>
                        </CardFooter>
                    </Card>
                    <h2>Drafts</h2>
                    <Card className="mb-3">
                        {drafts.length > 0 && drafts.map((draft) => (
                            <div key={draft.Id} >
                                <CardHeader><CardTitle>Draft # {draft.Id}</CardTitle></CardHeader>
                            </div>
                        )
                        )}
                    </Card>
                  
                </div>
            </main>)}
        </div>)
}

export default Request;