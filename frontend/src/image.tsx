
import Sidebar from "./sidebar"
import { useParams } from 'react-router-dom'
import { useEffect, useState } from 'react'
 
function ImagePage() {
    const { id } = useParams()
    const apiBase = 'https://localhost:44306'
    const [imageURL,setImageURL] = useState('')

    const getImageURL = async () => {
        const response = await fetch(apiBase + `/api/review/${id}/draft`, { headers: { "Accept": 'application/json' }, method: 'GET' });
        const data = await response.json();
        if (response.ok) {
            let tmp = apiBase + data.SiteOverview
            setImageURL(tmp)
    console.log(data.SiteOverview)}
    else {
        console.log("Error retrieving image URL")
    }

    }

    useEffect(() => {
        getImageURL()
    },[])
    return (
        <div className="bg-background w-full h-screen flex flex-1 flex-row justify-between text-center relative ">
            <aside className="max-w-[15%] left-0 sticky top-0 h-screen">
                <Sidebar />
            </aside>
            <main className="flex-col flex-1  justify-center px-5">
                <div className="w-full flex flex-row justify-center border-b-1 relative items-center mb-5">
                    <h2>Blueprint</h2>
                </div>
                <img src='https://localhost:44306/blueprints/blueprint_20260715161907.png' /> {/*Change src to imageURL */}

            </main>
        </div>
    )
}

export default ImagePage;