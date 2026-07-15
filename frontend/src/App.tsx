import { useState } from 'react'
import { Button } from './components/ui/button'
import { Accordion, AccordionItem, AccordionTrigger, AccordionContent } from './components/ui/accordion'
import { Field, FieldSet, FieldGroup, FieldTitle } from './components/ui/field'
import { Input } from './components/ui/input'
import { format } from "date-fns"
import { ChevronDownIcon } from "lucide-react"
import { Calendar } from "./components/ui/calendar"
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "./components/ui/popover"
import Sidebar from "./sidebar"





import './App.css'

function App() {
    const [date, setDate] = useState<Date>()
    const [venueName, setVenueName] = useState("")
    const [address1, setAddress1] = useState("")
    const [address2, setAddress2] = useState("")
    const [city, setCity] = useState("")
    const [zipcode, setZipcode] = useState(0)
    const [state, setCountry] = useState("")
    const [country, setState] = useState("")

    const [volunteerCount, setVolunteerCount] = useState(0)
    const [lineNumbers, setLineNumbers] = useState(0)
    const [mealPackageGoal, setMealPackageGoal] = useState(0)
    const [roomNotes, setRoomNotes] = useState("")
    const [loadingNotes, setLoadingNotes] = useState("")
    const [equipment, setEquipment] = useState("")
    const [constraints, setConstraints] = useState("")
    const [additionalNotes, setAdditionalNotes] = useState("")
    const [projectType, setProjectType] = useState("")
    //6






    return (
        <div className="bg-background w-full h-screen flex flex-1 flex-row justify-between text-center relative ">
            <aside className="max-w-[15%] left-0 sticky top-0 h-screen">
                <Sidebar />
            </aside>
            <main className="flex-col flex-1  justify-center px-5">
                <div className="w-full flex flex-row justify-center border-b-1 relative items-center mb-5">

                    <div className="flex items-center text-center justify-center w-full">
                        <h1 className="!text-accent">Site Plan Generator</h1>
                    </div>
                </div>
                <p>You can enter information about any venue below to generate a draft site plan</p>
                {/*Enter site plan request details*/}
                <div>
                    <Accordion className="flex  justify-center">
                        <FieldSet className="border-none p-o">
                            <FieldGroup>
                                <AccordionItem  >
                                    <AccordionTrigger className="text-center flex justify-center items-center">Venue Overview</AccordionTrigger>
                                    <AccordionContent className="">
                                        <Field>
                                            <FieldTitle>Venue Name</FieldTitle>
                                            <Input value={venueName} onInput={(e) => setVenueName(e.target.value)} />

                                        </Field>
                                        <Field>
                                            <FieldTitle>Date</FieldTitle>
                                            <Popover>
                                                <PopoverTrigger render={<Button variant={"outline"} data-empty={!date} className="w-[212px] justify-between text-left font-normal data-[empty=true]:text-muted-foreground">{date ? format(date, "PPP") : <span>Pick a date</span>}<ChevronDownIcon data-icon="inline-end" /></Button>} />
                                                <PopoverContent className="w-auto p-0" align="start">
                                                    <Calendar
                                                        mode="single"
                                                        selected={date}
                                                        onSelect={setDate}
                                                        defaultMonth={date}
                                                    />
                                                </PopoverContent>
                                            </Popover>
                                        </Field>
                                        <FieldGroup className="flex">
                                            <Field>
                                                <FieldTitle>Address Line 1</FieldTitle>
                                                <Input value={address1} onInput={(e) => setAddress1(e.target.value)} />

                                            </Field>
                                            <Field>
                                                <FieldTitle>Address Line 2</FieldTitle>
                                                <Input value={address2} onInput={(e) => setAddress2(e.target.value)} />

                                            </Field>
                                            <div className="flex-col flex">
                                                <div className="flex-row flex gap-5">

                                                    <Field>
                                                        <FieldTitle>City</FieldTitle>
                                                        <Input value={city} onInput={(e) => setCity(e.target.value)} />

                                                    </Field>
                                                    <Field>
                                                        <FieldTitle>State</FieldTitle>
                                                        <Input value={state} onInput={(e) => setState(e.target.value)} />

                                                    </Field>
                                                </div>

                                                <div className="flex-row flex gap-5">

                                                    <Field>
                                                        <FieldTitle>Zip Code</FieldTitle>
                                                        <Input value={zipcode} onInput={(e) => setZipcode(e.target.value)} />

                                                    </Field>
                                                    <Field>
                                                        <FieldTitle>Country</FieldTitle>
                                                        <Input value={country} onInput={(e) => setCountry(e.target.value)} />


                                                    </Field>
                                                </div>

                                            </div>
                                        </FieldGroup>


                                    </AccordionContent>
                                </ AccordionItem>
                            </FieldGroup>
                            <FieldGroup>
                                <AccordionItem >
                                    <AccordionTrigger>Numerics</AccordionTrigger>
                                    <AccordionContent>
                                        <FieldGroup className="flex-row flex justify-between px-4">
                                            <div>

                                                <FieldTitle>Volunteer Count</FieldTitle>
                                                <Input value={volunteerCount} onInput={(e) => setVolunteerCount(e.target.value)} />
                                            </div>
                                            <div>
                                                <FieldTitle>Number of Lines</FieldTitle>
                                                <Input value={lineNumbers} onInput={(e) => setLineNumbers(e.target.value)} />

                                            </div>

                                            <div>
                                                <FieldTitle>Meal Goal</FieldTitle>
                                                <Input value={mealPackageGoal} onInput={(e) => setMealPackageGoal(e.target.value)} />
                                            </div>



                                        </FieldGroup>



                                    </AccordionContent>
                                </ AccordionItem>
                            </FieldGroup>

                            <FieldGroup>
                                <AccordionItem >
                                    <AccordionTrigger> Notes</AccordionTrigger>
                                    <AccordionContent>
                                        <Field>
                                            <FieldTitle>Project Type</FieldTitle>
                                            <Input value={projectType} onInput={(e) => setProjectType(e.target.value)} />
                                        </Field>
                                        <Field>
                                            <FieldTitle>Room Notes</FieldTitle>
                                            <Input value={roomNotes} onInput={(e) => setRoomNotes(e.target.value)} />

                                        </Field>
                                        <Field>
                                            <FieldTitle>Loading Notes</FieldTitle>
                                            <Input value={loadingNotes} onInput={(e) => setLoadingNotes(e.target.value)} />
                                        </Field><Field>
                                            <FieldTitle>Available Equipment</FieldTitle>
                                            <Input value={equipment} onInput={(e) => setEquipment(e.target.value)} />
                                        </Field><Field>
                                            <FieldTitle>Special Constraints</FieldTitle>
                                            <Input value={constraints} onInput={(e) => setConstraints(e.target.value)} />
                                        </Field><Field>
                                            <FieldTitle>Additional Notes</FieldTitle>
                                            <Input value={additionalNotes} onInput={(e) => setAdditionalNotes(e.target.value)} />
                                        </Field>
                                        <Field>
                                            <FieldTitle>Upload File</FieldTitle>
                                            <Button className='max-w-[25%] bg-accent'>Choose file to upload</Button>
                                        </Field>

                                    </AccordionContent>
                                </ AccordionItem>
                            </FieldGroup>
                        </FieldSet>
                    </Accordion>
                </div>
                <div className="flex items-center justify-center pt-15">
                    <Button className="bg-primary mb-5">Generate Blueprint</Button>

                </div>

            </main>
        </div >
    )
}

export default App
