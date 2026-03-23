import { Header } from "@/app/components/Header";
import { Button } from "@/app/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/app/components/ui/card";
import { ArrowLeft, Mail, Code, Github } from "lucide-react";
import { useNavigate } from "react-router-dom";

export function AboutPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-secondary/20">
      {/* Decorative background pattern */}
      <div className="fixed inset-0 opacity-5 pointer-events-none">
        <div
          className="absolute inset-0"
          style={{
            backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
            backgroundSize: "40px 40px",
          }}
        />
      </div>

      <div className="container mx-auto py-8 px-4 relative">
        <Header />

        <div className="mt-8 space-y-6 max-w-4xl mx-auto">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => navigate('/')}
              className="gap-2"
            >
              <ArrowLeft className="size-4" />
              Back to Home
            </Button>
          </div>
          
          <div>
            <h2 className="text-3xl font-bold">About GM Buddy</h2>
            <p className="text-muted-foreground mt-2">
              Information about this project
            </p>
          </div>

          {/* Creator Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Code className="size-5" />
                About the Creator
              </CardTitle>
              <CardDescription>
                Who built this application
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">
                GM Buddy was built by Nathan Wolke, a software developer and tabletop RPG enthusiast.
              </p>
              <a
                href="https://github.com/nwolke/GM_Buddy"
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 text-sm text-primary hover:underline"
              >
                <Github className="size-4" />
                github.com/nwolke/GM_Buddy
              </a>
            </CardContent>
          </Card>

          {/* Contact Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Mail className="size-5" />
                Contact Information
              </CardTitle>
              <CardDescription>
                Get in touch
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              <a
                href="mailto:gmbuddy@outlook.com"
                className="flex items-center gap-2 text-sm text-primary hover:underline"
              >
                <Mail className="size-4" />
                gmbuddy@outlook.com
              </a>
              <a
                href="https://github.com/nwolke"
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-2 text-sm text-primary hover:underline"
              >
                <Github className="size-4" />
                github.com/nwolke
              </a>
            </CardContent>
          </Card>


        </div>
      </div>
    </div>
  );
}
