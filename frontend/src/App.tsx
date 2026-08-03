import { useState, useRef } from 'react'
import { Button } from './components/ui/button'
import { Accordion, AccordionItem, AccordionTrigger, AccordionContent } from './components/ui/accordion'
import { Field, FieldSet, FieldGroup, FieldTitle } from './components/ui/field'
import { Input } from './components/ui/input'
import { format } from "date-fns"
import { ChevronDownIcon, Upload } from "lucide-react"
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
    const [zipcode, setZipcode] = useState<number>()
    const [state, setState] = useState("")
    const [country, setCountry] = useState("")

    const [tableSizes, setTableSizes] = useState<number>()
    const [lineNumber, setLineNumber] = useState<number>()
    const [roomNotes, setRoomNotes] = useState("")
    const [loadingNotes, setLoadingNotes] = useState("")
    const [palettes, setPalettes] = useState<number>()
    const [clientName, setClientName] = useState("")
    const [additionalNotes, setAdditionalNotes] = useState("")

    const inputRef = useRef<HTMLInputElement>(null)
    const [selectedFile, setSelectedFile] = useState(null)
    const apiBase = 'https://localhost:44306';



    const attachFile = (event) => {
        const selected = event.target.files[0]
        if (selected) {
            setSelectedFile(selected)
        }
    }

    const createReq = async () => {
        const fullAddress = address1 + " " + address2 + " " + city + " " + state + " " + zipcode
        const response = await fetch(`${apiBase}/api/requests`, {
            headers: {
                "Accept": "application/json", "Content-Type": "application/json",
            }, method: "POST", body: JSON.stringify({
                ClientName: clientName,
                Deadline: date,
                VenueName: venueName,
                VenueAddress: fullAddress,
                NumberofPalettes: palettes,
                NumberofLines: lineNumber,
                TableSizes: tableSizes,
                RoomDimensions: roomNotes,
                LoadingNotes: loadingNotes,
                AdditionalNotes: additionalNotes,
                RoomBlueprintFilePath: selectedFile?.name

            })
        })
        const data = await response.json();
        if (response.ok) {
            console.log("Request creted")
        } else { console.log("Error creating request", data) }
    }

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
                                            <FieldTitle>Client Name</FieldTitle>
                                            <Input value={clientName} onInput={(e) => setClientName(e.target.value)} />

                                        </Field>

                                        <Field>
                                            <FieldTitle>Project Date</FieldTitle>
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
                                                <FieldTitle>Venue Name</FieldTitle>
                                                <Input value={venueName} onInput={(e) => setVenueName(e.target.value)} />

                                            </Field>
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
                                        <FieldGroup className="flex-row flex justify-center px-4 gap-[35%]">
                                            <div>

                                                <FieldTitle>Number of Palettes</FieldTitle>
                                                <Input value={palettes} onInput={(e) => setPalettes(e.target.value)} />
                                            </div>
                                            <div>
                                                <FieldTitle>Number of Lines</FieldTitle>
                                                <Input value={lineNumber} onInput={(e) => setLineNumber(e.target.value)} />

                                            </div>
                                            <div>

                                                <FieldTitle>Table Sizes</FieldTitle>
                                                <Input placeholder="Ex: 10,6" value={tableSizes} onInput={(e) => setTableSizes(e.target.value)} />
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
                                            <FieldTitle>Room Dimensions</FieldTitle>
                                            <Input placeholder="Ex: 10x10 or Half circle with a 100ft diameter" value={roomNotes} onInput={(e) => setRoomNotes(e.target.value)} />

                                        </Field>
                                        <Field>
                                            <FieldTitle>Loading Notes</FieldTitle>
                                            <Input value={loadingNotes} onInput={(e) => setLoadingNotes(e.target.value)} />
                                        </Field><Field>
                                            <FieldTitle>Additional Notes</FieldTitle>
                                            <Input placeholder='Enter notes/constraints' value={additionalNotes} onInput={(e) => setAdditionalNotes(e.target.value)} />
                                        </Field>

                                        <Field>
                                            <FieldTitle>Venue Floor Plan </FieldTitle>
                                            <div className="flex flex-row w-full items-center gap-10 mr-5">
                                                <Button className='max-w-[25%] bg-accent' onClick={() => {
                                                    inputRef.current.click()
                                                }}><Upload />Upload Venue Floor Plan</Button>
                                                <p className='mr-5'>(JPEG, JPG, PNG, PDF)</p>
                                                <input type="file" ref={inputRef} accept=".jpeg, .jpg, .png, .pdf" onChange={attachFile} className="hidden" />

                                            </div>
                                        </Field>

                                    </AccordionContent>
                                </ AccordionItem>
                            </FieldGroup>
                        </FieldSet>
                    </Accordion>
                </div>
                <div className="flex items-center justify-center pt-15">
                    <Button className="bg-primary mb-5" onClick={() => {
                        createReq()
                        //Add the selected filed into the blueprints folder
                    }}>Generate Blueprint</Button>

                </div>

            </main>
        </div >
    )
}

export default App
