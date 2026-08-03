import Sidebar from './sidebar'
import { useParams, Link } from 'react-router-dom'
import { useState, useEffect } from 'react'
import { Card, CardHeader, CardContent, CardFooter, CardDescription } from './components/ui/card'
import { Separator } from './components/ui/separator'
import { MapPin, TriangleAlert } from 'lucide-react'
import { Badge } from "./components/ui/badge"
import { Button } from './components/ui/button'
import { Accordion, AccordionItem, AccordionTrigger, AccordionContent } from './components/ui/accordion'
import { Textarea } from './components/ui/textarea'
import { Input } from './components/ui/input'
import { Field, FieldSet, FieldGroup, FieldTitle } from './components/ui/field'








function Request() {
    const { id } = useParams()
    const apiBase = 'https://localhost:44306'
    const [drafts, setDrafts] = useState([])
    const [req, setReq] = useState()
    const [draftEditID, setDraftEditID] = useState<number>(0)
    const [editNotes, setEditNotes] = useState<string>()


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

    const generateInitialDraft = async () => {
        if (req == null) return
        console.log("CCalling backend")
        const response = await fetch(apiBase + `/api/prompt/`, {
            headers: {
                "Accept": "application/json", "Content-Type": "application/json",
            }, method: "POST", body: JSON.stringify(req)
        })
        const data = await response.json();
        if (response.ok) {
            console.log(data)
        }
        else {
            console.log(data)
        }

    }

    const editDraft = async () => {
        console.log("Editing draft... ")
        if (!draftEditID) return
        const response = await fetch(`${apiBase}/api/review/${draftEditID}/draft/edit`, { headers: { "Accept": "application/json", "Content-Type": "application/json" }, method: "PUT", body: JSON.stringify(editNotes) })
        const data = await response.json();
        if (response.ok) {
            console.log(data)
        }
        else { console.log(data) }
    }

    const finalizeDraft = async (draftID) => {
        const response = await fetch(`${apiBase}/api/review/${draftID}/finalize`, { headers: { "Accept": "application/json" }, method: "POST" })
        const data = await response.json()
        if (response.ok) {
            //Navigate to finalized Drafts page
            console.log(data)
        } else {
            console.log(data)
        }
    }

    useEffect(() => {
        getReq()
        getDrafts();
    }, [])
    useEffect(() => {
        if (drafts.length<1) return
        setDraftEditID(drafts[drafts.length - 1].Id);
    }, [drafts])
    return (
        <div className="bg-background w-full h-screen flex flex-1 flex-row justify-between text-center relative ">
            <aside className="max-w-[15%] left-0 sticky top-0 h-screen">
                <Sidebar />
            </aside>
            {req && (<main className="flex-col flex-1  justify-center px-5">
                <h1>{req.VenueName}</h1>
                <div>
                    <h2>Initial Request</h2>
                    <Card>
                        <CardHeader className='flex justify-between pb-5'>
                            <div className='flex'>
                                <MapPin />
                                <p>{req.VenueAddress}</p></div>
                            <Badge variant='destructive'>{new Date((req.Deadline).slice(0, 10)).toLocaleDateString()}</Badge>
                        </CardHeader>
                        <CardContent>
                            <div className='flex justify-between px-15'>
                                <div>
                                    <h2> Table Sizes</h2>
                                    <p>{req.TableSizes}</p>
                                </div>
                                <div>
                                    <h2> Number of Lines</h2>

                                    <p>{req.NumberofLines}</p>
                                </div>
                                <div>
                                    <h2> Number of Palettes</h2>

                                    <p>{req.NumberofPalettes}</p>
                                </div>

                            </div>
                            <Separator className='my-5' />
                            <div className='grid grid-cols-2 gap-y-5'>
                                <div >
                                    <h2> Room Space Notes</h2>
                                    <p>{req.RoomDimensions}</p></div>
                                <div>
                                    <h2> Loading Notes</h2>
                                    <p>{req.LoadingNotes}</p></div>
                                <div>
                                    <h2> Bluprint</h2>
                                    <Link to={`/requests/${req.Id}/image/og`}>
                                        <p className='text-blue-600! underline decoration-blue-600!'>Room Blueprint Image</p>
                                    </Link>

                                </div>
                                <div>
                                    <h2> Additional Notes</h2>
                                    <p>{req.AdditionalNotes}</p></div>

                            </div>


                        </CardContent>

                    </Card>
                    {drafts.length > 0 ? (
                        <>
                            <h2>Drafts</h2>
                            <Accordion className="mb-3">
                                {drafts.length > 0 && drafts.map((draft, i) => (
                                    <div key={draft.Id} >
                                        <AccordionItem>
                                            <AccordionTrigger>Draft # {drafts.length - i}</AccordionTrigger>
                                            <AccordionContent>Checklist : {draft.PMReviewChecklist}</AccordionContent>
                                            <AccordionContent>Layout : {draft.RecommendedLayout}</AccordionContent>
                                            <AccordionContent>Risks : {draft.Risks}</AccordionContent>
                                            <AccordionContent>SupplyFlow : {draft.SupplyFlow}</AccordionContent>
                                            <AccordionContent>VolunteerFlow : {draft.VolunteerFlow}</AccordionContent>
                                            <AccordionContent className="flex justify-between mx-[25%]">
                                                <Link to={`/requests/${draft.Id}/image`}><p className='text-blue-600! underline decoration-blue-600!'>Show Blueprint Image</p></Link>
                                                <Button>Finalize Draft</Button>
                                            </AccordionContent>





                                        </AccordionItem>
                                    </div>
                                )
                                )}
                            </Accordion>
                        </>) : (
                        <>
                            <h2>Press here to generate an initial draft</h2>
                            <Button onClick={() => { generateInitialDraft() }}>Generate Draft</Button>
                        </>
                    )}
                    <h2>Edit Drafts</h2>
                    <Card className='flex  items-center'>
                        <CardDescription>By default the last created draft will be the one edited unless otherwise specified.</CardDescription>
                        <Field className="flex items-center justify-center">
                            <FieldTitle className='flex justify-center' >Select Draft</FieldTitle>
                            <Input className='w-10!' value={draftEditID} onInput={(e) => { setDraftEditID(drafts[e.target.value].Id) }} />
                        </Field>
                        <Field className="flex items-center justify-center">
                            <FieldTitle className='flex justify-center'>Edit Notes</FieldTitle>
                            <Textarea className='w-5/6!' placeholder="Enter the edits you want to make" value={editNotes} onInput={(e) => { setEditNotes(e.target.value) }} />
                        </Field>
                        <CardFooter>
                            <Button onClick={() => {
                                editDraft()
                                console.log(draftEditID)
                            }}>Submit Edits</Button>
                        </CardFooter>

                    </Card>
                </div>
            </main>)}
        </div>)
}

export default Request;